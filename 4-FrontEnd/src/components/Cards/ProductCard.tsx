export interface ProductCardProps {
    title: string;
    price: number;
    originalPrice?: number;
    discount?: number;
    imageUrl: string;
}

export function ProductCard({ title, price, originalPrice, discount, imageUrl }: ProductCardProps) {
    return (
        <div className="border border-gray-200 rounded-lg p-4 bg-white flex flex-col gap-2 relative shadow-sm hover:shadow-md transition">
            {discount && (
                <span className="absolute top-2 left-2 bg-orange-500 text-white text-xs font-bold px-2 py-1 rounded">
                    -{discount}%
                </span>
            )}

            <img src={imageUrl} alt={title} className="h-40 object-contain mx-auto mb-2" />

            <h3 className="text-sm text-gray-800 font-semibold h-10 overflow-hidden leading-tight">
                {title}
            </h3>

            <div className="mt-auto pt-2">
                {originalPrice && <p className="text-xs text-gray-400 line-through">R$ {originalPrice.toFixed(2)}</p>}
                <p className="text-xl font-bold text-gray-900">R$ {price.toFixed(2)}</p>
            </div>

            <button className="w-full bg-yellow-400 text-black py-2 rounded font-bold mt-3 hover:bg-yellow-500 flex items-center justify-center gap-2">
                🛒 Adicionar ao carrinho
            </button>
        </div>
    );
}