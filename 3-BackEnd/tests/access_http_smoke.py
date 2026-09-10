"""HTTP integration smoke against the isolated, seeded development API only."""
import json, urllib.request, urllib.error, http.cookiejar, uuid, os

BASE = 'http://localhost:5147/api/v1'
class Client:
    def __init__(self):
        self.opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
        self.token = None
    def call(self, path, method='GET', body=None, expected=200, tenant='1', csrf=True):
        headers = {'X-Tenant-Id': tenant, 'Content-Type': 'application/json'}
        if self.token and csrf: headers['X-CSRF-Token'] = self.token
        req = urllib.request.Request(BASE + path, data=json.dumps(body).encode() if body is not None else None, headers=headers, method=method)
        try:
            response = self.opener.open(req)
        except urllib.error.HTTPError as error:
            response = error
        raw = response.read().decode()
        assert response.status == expected, (path, response.status, raw[:500])
        return json.loads(raw) if raw else None
    def session(self):
        result = self.call('/session'); self.token = result['csrfToken']; return result

# Only synthetic local users are created. No external messages or payments.
admin=Client(); assert admin.call('/store/context')['companyName']=='Loja de demonstração'; admin.session()
admin.call('/session/admin/login','POST',{'username':'admin','password':os.environ['CLOUDSHOPPING_DEMO_PASSWORD']}); admin.session()
assert admin.session()['user']['permissions']==['*']
root='/tenants/1/backoffice'
profile=admin.call(root+'/profiles','POST',{'tenantId':1,'name':'Consulta teste '+uuid.uuid4().hex[:8]},expected=201)['id']
username='read-'+uuid.uuid4().hex[:10]
employee=admin.call(root+'/employees')[0]['id']
user=admin.call(root+'/users','POST',{'tenantId':1,'employeeId':employee,'username':username,'password':'Read-only-test-2026!'},expected=201)['id']
admin.call(root+'/profile-users','POST',{'tenantId':1,'profileId':profile,'employeeUserId':user},expected=201)
admin.call('/access/profiles/'+str(profile),'PUT',{'expected':[],'permissions':['catalog.read','finance.read']},expected=204)
staff=Client(); staff.session(); staff.call('/session/admin/login','POST',{'username':username,'password':'Read-only-test-2026!'}); staff.session()
staff.call('/products'); staff.call('/asaas/admin/payments')
staff.call('/products/1/stock/adjust','POST',{},expected=403)
staff.call('/asaas/admin/orders/1/refund','POST',{},expected=403)
staff.call('/asaas/connection','PUT',{'mode':'Direct','environment':'Sandbox'},expected=403)
staff.call('/access',expected=403); staff.call(root+'/users',expected=403)
staff.call('/products',tenant='2',expected=403)
admin.call('/access/profiles/'+str(profile),'PUT',{'expected':['catalog.read','finance.read'],'permissions':['finance.read']},expected=204)
staff.call('/products',expected=403); staff.call('/asaas/admin/payments')
admin.call('/access/profiles/'+str(profile),'PUT',{'expected':['finance.read'],'permissions':[]},expected=204)
staff.call('/asaas/admin/payments',expected=401)
# Two independent browser sessions; old cookie stops authorizing after revocation.
first=Client(); first.session(); email=uuid.uuid4().hex+'@example.test'
first.call('/session/register','POST',{'email':email,'password':'Before-change-2026!'}); first.session()
second=Client(); second.session(); second.call('/session/login','POST',{'username':email,'password':'Before-change-2026!'}); second.session()
assert len(first.call('/session/security/sessions'))==2
first.call('/session/security/sessions/revoke','POST',{'sessionId':None},expected=204)
second.call('/store/orders',expected=401)
first.call('/session/security/password','POST',{'currentPassword':'incorrect','newPassword':'After-change-2026!'},expected=400)
first.call('/session/security/password','POST',{'currentPassword':'Before-change-2026!','newPassword':'After-change-2026!'},expected=204)
first.call('/store/orders',expected=401)
first.session(); first.call('/session/login','POST',{'username':email,'password':'After-change-2026!'}); first.session()
first.call('/store/orders')
print('Access HTTP smoke passed: read-only, stock/refund/configuration denial, live revocation, tenant isolation, session revocation and password rotation.')
