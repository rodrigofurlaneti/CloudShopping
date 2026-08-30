import { ReactNode } from 'react';

export function StoreLayout({ children }: { children: ReactNode }) {
    return (
        <div className="min-h-screen bg-gray-50 font-sans">
            <header className="bg-[#0f172a] text-white p-4 flex justify-between items-center px-8">
                <div className="text-2xl font-bold flex items-center gap-2 text-orange-500">
                    🛒 <span className="text-white">MinhaLoja</span>
                </div>

                <div className="flex w-1/2 rounded bg-white overflow-hidden">
                    <select className="bg-gray-100 text-black px-4 py-2 outline-none border-r">
                        <option>Todas as categorias</option>
                    </select>
                    <input type="text" placeholder="O que você está procurando?" className="w-full px-4 text-black outline-none" />
                    <button className="bg-yellow-400 px-6 text-black font-bold">🔍</button>
                </div>

                <div className="flex gap-6 items-center">
                    <div className="text-sm text-right">
                        <p>Olá, Rodrigo</p>
                        <p className="font-bold">Minha Conta ⌄</p>
                    </div>
                    <div className="flex items-center gap-2 font-bold">
                        <span className="text-2xl">🛒</span>
                        <span className="bg-orange-500 text-white rounded-full px-2 text-xs absolute top-3 right-6">3</span>
                        Meu Carrinho
                    </div>
                </div>
            </header>

            <nav className="bg-[#1e293b] text-white px-8 py-2 flex gap-6 text-sm font-semibold">
                <span>≡ Todos os Departamentos</span>
                <span>Ofertas do Dia</span>
                <span>Mais Vendidos</span>
                <span>Eletrônicos</span>
                <span>Casa e Cozinha</span>
            </nav>

            <main className="p-8 max-w-[1400px] mx-auto">
                {children}
            </main>
        </div>
    );
}