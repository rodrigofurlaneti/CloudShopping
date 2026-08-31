import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { TenantAuthService } from '../../services/authService';

export function RegisterCompany() {
    const navigate = useNavigate();

    const [companyName, setCompanyName] = useState('');
    const [domain, setDomain] = useState('');
    const [adminName, setAdminName] = useState('');
    const [adminCpf, setAdminCpf] = useState('');
    const [adminEmail, setAdminEmail] = useState('');
    const [adminPhone, setAdminPhone] = useState('');
    const [adminUsername, setAdminUsername] = useState('');
    const [adminPassword, setAdminPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');

    const [loading, setLoading] = useState(false);
    const [errorMessage, setErrorMessage] = useState('');
    const [successMessage, setSuccessMessage] = useState('');

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrorMessage('');
        setSuccessMessage('');

        if (adminPassword !== confirmPassword) {
            setErrorMessage('As senhas não conferem.');
            return;
        }

        setLoading(true);
        try {
            const result = await TenantAuthService.registerCompany({
                companyName,
                domain: domain.trim() || undefined,
                adminName,
                adminCpf: adminCpf.replace(/\D/g, ''),
                adminEmail,
                adminPhone: adminPhone.trim() || undefined,
                adminUsername,
                adminPassword,
            });

            setSuccessMessage(`Empresa "${result.companyName}" criada com sucesso! Usuário administrador: ${result.username}.`);

            setTimeout(() => {
                navigate(`/admin/login?username=${encodeURIComponent(result.username)}`);
            }, 1800);
        } catch (error) {
            setErrorMessage(error instanceof Error ? error.message : 'Não foi possível cadastrar a empresa.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-slate-900 flex items-center justify-center px-4 py-10">
            <div className="max-w-lg w-full bg-slate-800 rounded-2xl shadow-xl border border-slate-700 p-8">

                {/* Cabeçalho / Logo */}
                <div className="text-center mb-8">
                    <div className="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-orange-600 text-white font-black text-xl mb-3 shadow-inner">
                        CS
                    </div>
                    <h1 className="text-2xl font-bold text-white tracking-tight">Crie sua loja no CloudShopping</h1>
                    <p className="text-sm text-slate-400 mt-1">Cadastre sua empresa e comece a vender em minutos.</p>
                </div>

                {errorMessage && (
                    <div className="mb-4 bg-red-500/10 border border-red-500/50 text-red-400 text-xs p-3 rounded-lg text-center font-medium">
                        {errorMessage}
                    </div>
                )}

                {successMessage && (
                    <div className="mb-4 bg-emerald-500/10 border border-emerald-500/50 text-emerald-400 text-xs p-3 rounded-lg text-center font-medium">
                        {successMessage}
                    </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-5">
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <div className="sm:col-span-2">
                            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">
                                Nome da Empresa
                            </label>
                            <input
                                type="text"
                                required
                                value={companyName}
                                onChange={(e) => setCompanyName(e.target.value)}
                                placeholder="Minha Loja Ltda."
                                className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-3 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-orange-500 focus:ring-1 focus:ring-orange-500 transition-all"
                            />
                        </div>

                        <div className="sm:col-span-2">
                            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">
                                Domínio (opcional)
                            </label>
                            <input
                                type="text"
                                value={domain}
                                onChange={(e) => setDomain(e.target.value)}
                                placeholder="minhaloja.com"
                                className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-3 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-orange-500 focus:ring-1 focus:ring-orange-500 transition-all"
                            />
                        </div>
                    </div>

                    <div className="border-t border-slate-700/60 pt-4">
                        <p className="text-xs font-semibold uppercase tracking-wider text-slate-400 mb-3">
                            Administrador da empresa
                        </p>

                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                            <div className="sm:col-span-2">
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">
                                    Nome completo
                                </label>
                                <input
                                    type="text"
                                    required
                                    value={adminName}
                                    onChange={(e) => setAdminName(e.target.value)}
                                    placeholder="Seu nome"
                                    className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-3 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-orange-500 focus:ring-1 focus:ring-orange-500 transition-all"
                                />
                            </div>

                            <div>
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">
                                    CPF
                                </label>
                                <input
                                    type="text"
                                    required
                                    inputMode="numeric"
                                    maxLength={11}
                                    value={adminCpf}
                                    onChange={(e) => setAdminCpf(e.target.value.replace(/\D/g, '').slice(0, 11))}
                                    placeholder="Somente números"
                                    className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-3 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-orange-500 focus:ring-1 focus:ring-orange-500 transition-all"
                                />
                            </div>

                            <div>
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">
                                    Telefone (opcional)
                                </label>
                                <input
                                    type="tel"
                                    value={adminPhone}
                                    onChange={(e) => setAdminPhone(e.target.value)}
                                    placeholder="(11) 90000-0000"
                                    className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-3 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-orange-500 focus:ring-1 focus:ring-orange-500 transition-all"
                                />
                            </div>

                            <div className="sm:col-span-2">
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">
                                    E-mail
                                </label>
                                <input
                                    type="email"
                                    required
                                    value={adminEmail}
                                    onChange={(e) => setAdminEmail(e.target.value)}
                                    placeholder="admin@sualoja.com"
                                    className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-3 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-orange-500 focus:ring-1 focus:ring-orange-500 transition-all"
                                />
                            </div>

                            <div>
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">
                                    Usuário de acesso
                                </label>
                                <input
                                    type="text"
                                    required
                                    value={adminUsername}
                                    onChange={(e) => setAdminUsername(e.target.value)}
                                    placeholder="admin"
                                    className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-3 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-orange-500 focus:ring-1 focus:ring-orange-500 transition-all"
                                />
                            </div>

                            <div>
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">
                                    Senha
                                </label>
                                <input
                                    type="password"
                                    required
                                    minLength={6}
                                    value={adminPassword}
                                    onChange={(e) => setAdminPassword(e.target.value)}
                                    placeholder="••••••••"
                                    className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-3 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-orange-500 focus:ring-1 focus:ring-orange-500 transition-all"
                                />
                            </div>

                            <div className="sm:col-span-2">
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">
                                    Confirmar senha
                                </label>
                                <input
                                    type="password"
                                    required
                                    minLength={6}
                                    value={confirmPassword}
                                    onChange={(e) => setConfirmPassword(e.target.value)}
                                    placeholder="••••••••"
                                    className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-3 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-orange-500 focus:ring-1 focus:ring-orange-500 transition-all"
                                />
                            </div>
                        </div>
                    </div>

                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full bg-orange-600 hover:bg-orange-500 active:scale-[0.99] text-white font-bold py-3 px-4 rounded-lg text-sm shadow-lg shadow-orange-600/20 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
                    >
                        {loading ? (
                            <span className="inline-block w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
                        ) : (
                            "Criar minha empresa"
                        )}
                    </button>
                </form>

                <div className="mt-8 text-center border-t border-slate-700/60 pt-4">
                    <p className="text-xs text-slate-500">
                        Já tem uma empresa cadastrada?{' '}
                        <Link to="/admin/login" className="text-orange-400 hover:underline">
                            Entrar no Backoffice
                        </Link>
                    </p>
                </div>

            </div>
        </div>
    );
}

export default RegisterCompany;
