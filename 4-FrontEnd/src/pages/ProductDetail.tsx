import { useState } from 'react';
import { StoreLayout } from '../layouts/StoreLayout';

export function ProductDetail() {
    // Exemplo de estado para simulação de galeria e quantidade
    const [selectedImage, setSelectedImage] = useState(
        "https://via.placeholder.com/500x500?text=Produto+Principal"
    );
    const [quantity, setQuantity] = useState(1);

    const thumbnails = [
        "https://via.placeholder.com/100x100?text=Img+1",
        "https://via.placeholder.com/100x100?text=Img+2",
        "https://via.placeholder.com/100x100?text=Img+3",
        "https://via.placeholder.com/100x100?text=Img+4",
    ];

    return (
        <StoreLayout>
            <div className="max-w-7xl mx-auto px-4 py-4 bg-white text-gray-800">

                {/* Breadcrumb / Caminho */}
                <nav className="text-xs text-gray-500 mb-4">
                    Casa <span className="mx-1">&gt;</span> Banho <span className="mx-1">&gt;</span> Instalações e Acessórios de Banheiro <span className="mx-1">&gt;</span> Chuveiros
                </nav>

                <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">

                    {/* COLUNA 1: Imagens (Esquerda - 5 colunas) */}
                    <div className="lg:col-span-5 flex flex-col items-center sticky top-4 h-fit">
                        <div className="w-full h-[450px] flex items-center justify-center border border-gray-100 rounded-lg mb-4 overflow-hidden relative group">
                            <img
                                src={selectedImage}
                                alt="Produto em destaque"
                                className="max-h-full object-contain transition-transform duration-300 group-hover:scale-105"
                            />
                            <span className="absolute bottom-2 text-xs text-blue-600 bg-white/80 px-2 py-1 rounded cursor-pointer hover:underline">
                                Clique para ver a imagem completa
                            </span>
                        </div>

                        {/* Miniaturas */}
                        <div className="flex gap-2 overflow-x-auto w-full pb-2">
                            {thumbnails.map((thumb, index) => (
                                <button
                                    key={index}
                                    onClick={() => setSelectedImage(thumb)}
                                    className="w-16 h-16 border-2 border-gray-200 rounded-md overflow-hidden focus:border-amber-500 hover:border-amber-400 flex-shrink-0"
                                >
                                    <img src={thumb} alt={`Miniatura ${index + 1}`} className="w-full h-full object-cover" />
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* COLUNA 2: Informações Técnicas e Título (Centro - 4 colunas) */}
                    <div className="lg:col-span-4 border-b lg:border-b-0 lg:border-r border-gray-200 lg:pr-6">
                        <h1 className="text-2xl font-medium text-gray-900 leading-snug mb-2">
                            Duchinha Gatilho punho para Ducha Manual higiênica Activa Cromada Deca - 4906003
                        </h1>

                        <a href="#" className="text-sm text-teal-700 hover:underline hover:text-amber-600 block mb-2">
                            Marca: Deca
                        </a>

                        {/* Avaliações */}
                        <div className="flex items-center gap-2 mb-3 text-sm">
                            <div className="text-amber-500 font-bold">4,2 ★★★★☆</div>
                            <span className="text-blue-600 hover:underline cursor-pointer">(69)</span>
                            <span className="text-xs text-gray-500">| Mais de 50 compras no mês passado</span>
                        </div>

                        <hr className="my-3 border-gray-200" />

                        {/* Bloco de Preço Detalhado */}
                        <div className="mb-4">
                            <div className="flex items-baseline gap-1">
                                <span className="text-xs align-super font-semibold">R$</span>
                                <span className="text-3xl font-medium">112</span>
                                <span className="text-xs align-super font-semibold">19</span>
                            </div>
                            <p className="text-xs text-gray-600">à vista no Pix ou NuPay (5% off)</p>
                            <p className="text-xs text-gray-600">ou <strong className="text-gray-900">R$ 118,10</strong> em até <strong className="text-gray-900">3x de R$ 39,38</strong> sem juros</p>
                        </div>

                        {/* Ícones de Benefícios */}
                        <div className="grid grid-cols-3 gap-2 py-4 border-y border-gray-200 text-center mb-6">
                            <div className="flex flex-col items-center text-[11px] text-blue-800">
                                <span className="text-lg mb-1">🛡️</span> Pagamentos e Segurança
                            </div>
                            <div className="flex flex-col items-center text-[11px] text-blue-800">
                                <span className="text-lg mb-1">📦</span> Enviado pela Amazon
                            </div>
                            <div className="flex flex-col items-center text-[11px] text-blue-800">
                                <span className="text-lg mb-1">↩️</span> Política de devolução
                            </div>
                        </div>

                        {/* Especificações / Tabela */}
                        <div className="mb-6">
                            <h3 className="text-sm font-bold text-gray-900 mb-2">Especificações do produto</h3>
                            <div className="grid grid-cols-2 text-xs py-1 border-b border-gray-100">
                                <span className="font-bold text-gray-600">Marca</span>
                                <span>Deca</span>
                            </div>
                            <div className="grid grid-cols-2 text-xs py-1 border-b border-gray-100">
                                <span className="font-bold text-gray-600">Estilo</span>
                                <span>Clássico</span>
                            </div>
                            <div className="grid grid-cols-2 text-xs py-1 border-b border-gray-100">
                                <span className="font-bold text-gray-600">Forma</span>
                                <span>Cilíndrico</span>
                            </div>
                            <div className="grid grid-cols-2 text-xs py-1 border-b border-gray-100">
                                <span className="font-bold text-gray-600">Tipo de Acabamento</span>
                                <span>Cromado</span>
                            </div>
                        </div>

                        {/* Sobre este item */}
                        <div>
                            <h3 className="text-sm font-bold text-gray-900 mb-2">Sobre este item</h3>
                            <ul className="list-disc list-inside text-xs space-y-1.5 text-gray-700">
                                <li>Duchinha com gatilho punho compatível com a Ducha Higiênica Manual.</li>
                                <li>Produzido com materiais de alta qualidade e durabilidade padrão Deca.</li>
                                <li>Fácil instalação e design moderno para o seu banheiro.</li>
                            </ul>
                        </div>
                    </div>

                    {/* COLUNA 3: Caixa de Compra / Buybox (Direita - 3 colunas) */}
                    <div className="lg:col-span-3">
                        <div className="border border-gray-300 rounded-lg p-4 shadow-sm bg-gray-50/50">

                            <div className="text-xl font-medium text-gray-900 mb-1">
                                R$ 112<span className="text-xs align-super">19</span>
                            </div>

                            <p className="text-xs text-green-700 font-semibold mb-3">
                                Entrega GRÁTIS <span className="text-gray-800 font-normal">Quinta-feira, 3 de Setembro</span> no seu primeiro pedido.
                            </p>

                            <p className="text-xs text-gray-600 mb-4">
                                📍 Entregue em São Paulo. <a href="#" className="text-blue-600 hover:underline">Atualizar local</a>
                            </p>

                            <p className="text-lg font-medium text-emerald-700 mb-4">
                                Em estoque
                            </p>

                            {/* Seletor de Quantidade */}
                            <div className="mb-4">
                                <label className="block text-xs text-gray-600 mb-1">Quantidade:</label>
                                <select
                                    value={quantity}
                                    onChange={(e) => setQuantity(Number(e.target.value))}
                                    className="w-full bg-gray-100 border border-gray-300 rounded-lg px-3 py-1.5 text-sm shadow-inner focus:outline-none focus:ring-2 focus:ring-amber-400"
                                >
                                    {[1, 2, 3, 4, 5].map((num) => (
                                        <option key={num} value={num}>{num}</option>
                                    ))}
                                </select>
                            </div>

                            {/* Botões de Ação */}
                            <div className="space-y-2 mb-4">
                                <button className="w-full bg-[#ffd814] hover:bg-[#f7ca00] active:scale-[0.99] text-gray-900 text-sm font-medium py-2 rounded-full shadow-sm transition-colors border border-[#fcd200]">
                                    Adicionar ao carrinho
                                </button>
                                <button className="w-full bg-[#ffa41c] hover:bg-[#fa8900] active:scale-[0.99] text-gray-900 text-sm font-medium py-2 rounded-full shadow-sm transition-colors border border-[#ff8f00]">
                                    Comprar agora
                                </button>
                            </div>

                            {/* Informações de Vendedor e Envio */}
                            <div className="text-xs space-y-2 text-gray-600 border-t border-gray-200 pt-3">
                                <div className="grid grid-cols-2">
                                    <span className="text-gray-500">Enviado por</span>
                                    <span className="text-blue-600">Amazon</span>
                                </div>
                                <div className="grid grid-cols-2">
                                    <span className="text-gray-500">Vendido por</span>
                                    <span className="text-blue-600">conexões cavagnoli</span>
                                </div>
                                <div className="grid grid-cols-2">
                                    <span className="text-gray-500">Devolução</span>
                                    <span className="text-blue-600">Elegível para Devolução...</span>
                                </div>
                            </div>

                        </div>
                    </div>

                </div>
            </div>
        </StoreLayout>
    );
}

export default ProductDetail;