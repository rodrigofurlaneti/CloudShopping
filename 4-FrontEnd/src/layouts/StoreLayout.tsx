import { useState, useEffect, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { DepartmentService, type Department } from '../services/api';

export function StoreLayout({ children }: { children: ReactNode }) {
    const navigate = useNavigate();
    const [departments, setDepartments] = useState<Department[]>([]);
    const [clickCount, setClickCount] = useState(0);

    useEffect(() => {
        const loadDepartments = async () => {
            try {
                const data = await DepartmentService.getAll();
                setDepartments(data);
            } catch (error) {
                console.error("Falha ao carregar departamentos:", error);
            }
        };

        loadDepartments();
    }, []);

    // Gatilho secreto: 3 cliques rápidos no logotipo redirecionam para o Backoffice
    const handleLogoClick = () => {
        const newCount = clickCount + 1;
        setClickCount(newCount);

        if (newCount === 3) {
            navigate('/admin/login');
            setClickCount(0);
        }

        setTimeout(() => setClickCount(0), 1000);
    };

    return (
        <div className="min-h-screen bg-[#f1f5f9] font-sans flex flex-col justify-between">
            <div>
                <header className="bg-[#0f172a] text-white p-4 flex justify-between items-center px-8">
                    {/* Logotipo com o gatilho secreto do Backoffice (3 cliques) */}
                    <div
                        onClick={handleLogoClick}
                        className="text-2xl font-bold flex items-center gap-2 text-orange-500 cursor-pointer select-none"
                        title="MinhaLoja"
                    >
                        🛒 <span className="text-white">MinhaLoja</span>
                    </div>

                    <div className="flex w-1/2 rounded bg-white overflow-hidden">
                        <select className="bg-gray-100 text-black px-4 py-2 outline-none border-r max-w-[200px] truncate cursor-pointer">
                            <option value="">Todas as categorias</option>
                            {departments.map(dept => (
                                <option key={dept.id} value={dept.slug}>{dept.name}</option>
                            ))}
                        </select>
                        <input type="text" placeholder="O que você está procurando?" className="w-full px-4 text-black outline-none" />
                        <button className="bg-yellow-400 px-6 text-black font-bold hover:bg-yellow-500 transition-colors">🔍</button>
                    </div>

                    <div className="flex gap-6 items-center">
                        <div className="text-sm text-right cursor-pointer">
                            <p className="font-bold hover:text-gray-300">Olá, faça seu login</p>
                        </div>
                        <div className="flex items-center gap-2 font-bold cursor-pointer hover:text-gray-300">
                            <div className="relative">
                                <span className="text-2xl">🛒</span>
                                <span className="bg-orange-500 text-white rounded-full px-1.5 py-0.5 text-[10px] absolute -top-1 -right-2">0</span>
                            </div>
                            Meu Carrinho
                        </div>
                    </div>
                </header>

                <nav id="Departamentos" className="bg-[#1e293b] text-white px-8 py-2 flex gap-6 text-sm font-semibold items-center overflow-x-auto whitespace-nowrap scrollbar-hide">
                    <span className="cursor-pointer hover:text-yellow-400 transition-colors">≡ Todos os Departamentos</span>
                    <span className="cursor-pointer hover:text-yellow-400 transition-colors">Ofertas do Dia</span>
                    <span className="cursor-pointer hover:text-yellow-400 transition-colors">Mais Vendidos</span>

                    {departments.slice(0, 7).map(dept => (
                        <span key={dept.id} className="cursor-pointer hover:text-yellow-400 transition-colors">
                            {dept.name}
                        </span>
                    ))}
                </nav>

                <main className="w-full px-8 py-8 mx-auto">
                    {children}
                </main>
            </div>

            {/* Rodapé discreto com acesso alternativo ao Backoffice */}
            <footer className="bg-[#0f172a] text-slate-400 py-6 px-8 text-xs border-t border-slate-800 flex justify-between items-center">
                <p>© 2026 CloudShopping. Todos os direitos reservados.</p>
                <button
                    onClick={() => navigate('/admin/login')}
                    className="opacity-20 hover:opacity-100 transition-opacity text-slate-500 hover:text-orange-400 flex items-center gap-1 cursor-pointer"
                    title="Acesso Restrito"
                >
                    <span>🔒</span> Backoffice
                </button>
            </footer>
        </div>
    );
}

export default StoreLayout;