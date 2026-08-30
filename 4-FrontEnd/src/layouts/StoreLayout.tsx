import { useState, useEffect, type ReactNode } from 'react';
import { DepartmentService, type Department } from '../services/api';

export function StoreLayout({ children }: { children: ReactNode }) {
    const [departments, setDepartments] = useState<Department[]>([]);

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

    return (
        <div className="min-h-screen bg-[#f1f5f9] font-sans">
            <header className="bg-[#0f172a] text-white p-4 flex justify-between items-center px-8">
                <div className="text-2xl font-bold flex items-center gap-2 text-orange-500 cursor-pointer">
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
                        <p>Olá, Rodrigo</p>
                        <p className="font-bold hover:text-gray-300">Minha Conta ⌄</p>
                    </div>
                    <div className="flex items-center gap-2 font-bold cursor-pointer hover:text-gray-300">
                        <div className="relative">
                            <span className="text-2xl">🛒</span>
                            <span className="bg-orange-500 text-white rounded-full px-1.5 py-0.5 text-[10px] absolute -top-1 -right-2">3</span>
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

            {/* main atualizado: w-full garante o preenchimento total da tela mantendo o padding (px-8) igual ao do header */}
            <main className="w-full px-8 py-8 mx-auto">
                {children}
            </main>
        </div>
    );
}