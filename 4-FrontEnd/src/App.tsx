import { StoreLayout } from './layouts/StoreLayout';
import { ProductCard } from './components/Cards/ProductCard';

function App() {
    const mockProduct = {
        title: "Smartphone iPhone 14 128GB - Preto",
        price: 4499.00,
        originalPrice: 5999.00,
        discount: 25,
        imageUrl: "https://via.placeholder.com/150"
    };

    return (
        <StoreLayout>
            <div className="grid grid-cols-3 gap-6 mb-10">
                <div className="bg-orange-500 h-72 rounded-2xl p-6 text-white shadow-md">
                    <h2 className="text-4xl font-extrabold">Ofertas imperdíveis</h2>
                    <p className="text-6xl font-black mt-4">60% OFF</p>
                </div>
                <div className="bg-green-500 h-72 rounded-2xl p-6 text-white shadow-md">
                    <h2 className="text-3xl font-bold">Tecnologia para o seu dia a dia</h2>
                </div>
                <div className="bg-blue-600 h-72 rounded-2xl p-6 text-white shadow-md">
                    <h2 className="text-3xl font-bold">Casa e Cozinha</h2>
                </div>
            </div>

            <div className="flex justify-between items-end mb-4">
                <h2 className="text-2xl font-black text-gray-800">Ofertas em Destaque</h2>
                <a href="#" className="text-blue-600 font-bold text-sm">Ver mais &gt;</a>
            </div>

            <div className="grid grid-cols-5 gap-4">
                <ProductCard {...mockProduct} />
                <ProductCard {...mockProduct} title="Notebook Dell Inspiron i5" price={2799.00} originalPrice={3499.00} discount={20} />
                <ProductCard {...mockProduct} title="Air Fryer 4L Philco" price={349.00} originalPrice={499.00} discount={30} />
                <ProductCard {...mockProduct} title="Smartwatch Samsung Galaxy" price={1099.00} originalPrice={1299.00} discount={15} />
                <ProductCard {...mockProduct} title="Liquificador Mondial" price={154.90} originalPrice={199.00} discount={22} />
            </div>
        </StoreLayout>
    );
}

export default App;