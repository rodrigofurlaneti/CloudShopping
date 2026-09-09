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

c = Client(); assert c.call('/store/context')['companyName'] == 'Loja de demonstração'; c.session()
c.call('/store/cart', expected=401)
c.call('/session/guest', 'POST', {}, expected=400, csrf=False)
c.call('/session/admin/login', 'POST', {'username':'admin','password':'invalid-password'}, expected=401)
c.call('/session/guest', 'POST', {}); c.session()
c.call('/store/cart', expected=403, tenant='2')
products = c.call('/store/products')['items']; product = products[0]
c.call('/store/cart/items', 'POST', {'productId':product['id'], 'quantity':1})
email=str(uuid.uuid4())+'@example.test'
c.call('/store/profile', 'PUT', {'email':email,'name':'Cliente de teste','type':'B2C','taxId':'52998224725'}, expected=204)
c.call('/session/register','POST',{'email':email,'password':'Test-only-password-2026!'})
assert c.session()['user']['isGuest'] is False
address = c.call('/store/addresses', 'POST', {'street':'Rua de teste','number':'10','neighborhood':'Centro','city':'São Paulo','state':'SP','zipCode':'01001000'})
shipping = c.call('/store/shipping-options?addressId='+str(address['id']))[0]
quote = c.call('/store/checkout/preview', 'POST', {'addressId':address['id'],'shippingId':shipping['id']})
attempt = {'key':str(uuid.uuid4()),'token':quote['token']}
order = c.call('/store/checkout/confirm', 'POST', attempt)
assert order['totalAmount'] == quote['total']
assert c.call('/store/checkout/confirm', 'POST', attempt)['id'] == order['id']
other = Client(); other.session(); other.call('/session/guest','POST',{}); other.session()
other.call('/store/orders/'+str(order['id']), expected=404)
c.call('/asaas/connection', expected=403)
other.call('/asaas/orders/'+str(order['id']), expected=404)
assert c.call('/asaas/orders/'+str(order['id']))['configured'] is False
c.call('/asaas/orders/'+str(order['id']), 'POST', {'method':'PIX'}, expected=400, csrf=False)
c.call('/asaas/orders/'+str(order['id']), 'POST', {'method':'INVALID'}, expected=400)
c.call('/asaas/admin/payments', expected=403)
c.call('/store/orders/'+str(order['id'])+'/cancel','POST',{}, expected=204)
c.call('/store/orders/'+str(order['id'])+'/cancel','POST',{}, expected=204)
c.call('/session/logout','POST',{}, expected=204); assert c.session()['user'] is None
c.call('/store/cart', expected=401)
print('PASS: anonymous access, CSRF, invalid login, tenant isolation, checkout, replay, ownership, cancellation, logout')

# Login proves ownership before merging an existing visitor cart.
other.call('/store/cart/items','POST',{'productId':product['id'],'quantity':1})
other.call('/session/login','POST',{'username':email,'password':'Test-only-password-2026!'})
other.session(); assert other.call('/store/cart')['items'][0]['quantity'] == 1
other.call('/store/orders/'+str(order['id']))
other.call('/session/logout','POST',{},expected=204)
admin_password=os.environ.get('CLOUDSHOPPING_DEMO_PASSWORD')
if not admin_password: raise RuntimeError('Set CLOUDSHOPPING_DEMO_PASSWORD for the isolated demo administrator test.')
admin=Client();admin.session()
admin.call('/session/admin/login','POST',{'username':'admin','password':admin_password})
assert admin.session()['user']['role']=='Administrator'
admin.call('/products?page=1&pageSize=10')
admin.call('/store/admin/shipping-options')
assert admin.call('/asaas/connection')['configured'] is False
assert admin.call('/asaas/admin/payments') == []
admin.call('/asaas/orders/'+str(order['id']), expected=403)
print('PASS: Asaas customer ownership, administrator isolation, unconfigured state, CSRF and method validation')
admin.call('/store/cart',expected=403)
admin.call('/orders/1/cancel','POST',{},expected=409)
admin.call('/session/logout','POST',{},expected=204)
print('PASS: registration preserves guest identity, customer login merges cart, administrator login, role isolation, legacy payment guards')
