import { AccountSecurityPanel } from '../../components/AccountSecurityPanel';
import { BackofficeLayout } from '../../layouts/BackofficeLayout';
export function SecurityPage() {
    return <BackofficeLayout><div className="commerce-admin"><div className="form-card"><h1>Meu acesso</h1><AccountSecurityPanel /></div></div></BackofficeLayout>;
}
