import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

export function AdminLogin() {
    const navigate = useNavigate();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [errorMessage, setErrorMessage] = useState('');

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrorMessage('');
        setLoading(true);

        try {
            // Exemplo de integração futura com a sua API de Backoffice/Auth
            // const response = await AuthService.login({ email, password });

            // Simulação de sucesso de autenticação
            await new Promise(resolve => setTimeout(resolve, 1000));

            // Redireciona para o painel administrativo após logar
            navigate('/admin/dashboard');
        } catch (error) {
            setErrorMessage('Credenciais inválidas. Verifique seu e-mail e senha.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-slate-900 flex items-center justify-center px-4">
            <div className="max-w-md w-full bg-slate-800 rounded-2xl shadow-xl border border-slate-700 p-8">

                {/* Cabeçalho / Logo */}
                <div className="text-center mb-8">
                    <div className="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-orange-600 text-white font-black text-xl mb-3 shadow-inner">
                        CS
                    </div>
                    <h1 className="text-2xl font-bold text-white tracking-tight">CloudShopping</h1>
                    <p className="text-sm text-slate-400 mt-1">Painel de Gestão Backoffice</p>
                </div>

                {/* Exibição de Erro, se houver */}
                {errorMessage && (
                    <div className="mb-4 bg-red-500/10 border border-red-500/50 text-red-400 text-xs p-3 rounded-lg text-center font-medium">
                        {errorMessage}
                    </div>
                )}

                {/* Formulário de Acesso */}
                <form onSubmit={handleLogin} className="space-y-5">
                    <div>
                        <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">
                            E-mail Corporativo
                        </label>
                        <input
                            type="email"
                            required
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="admin@sualoja.com"
                            className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-3 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-orange-500 focus:ring-1 focus:ring-orange-500 transition-all"
                        />
                    </div>

                    <div>
                        <div className="flex justify-between items-center mb-1.5">
                            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300">
                                Senha
                            </label>
                            <a href="#" className="text-xs text-orange-400 hover:underline">
                                Esqueceu a senha?
                            </a>
                        </div>
                        <input
                            type="password"
                            required
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            placeholder="••••••••"
                            className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-3 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-orange-500 focus:ring-1 focus:ring-orange-500 transition-all"
                        />
                    </div>

                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full bg-orange-600 hover:bg-orange-500 active:scale-[0.99] text-white font-bold py-3 px-4 rounded-lg text-sm shadow-lg shadow-orange-600/20 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
                    >
                        {loading ? (
                            <span className="inline-block w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
                        ) : (
                            "Entrar no Backoffice"
                        )}
                    </button>
                </form>

                {/* Rodapé do Card */}
                <div className="mt-8 text-center border-t border-slate-700/60 pt-4">
                    <p className="text-xs text-slate-500">
                        Área restrita a lojistas e administradores autorizados.
                    </p>
                </div>

            </div>
        </div>
    );
}

export default AdminLogin;