import { StoreLayout } from './layouts/StoreLayout';
import { ProductCard } from './components/Cards/ProductCard';
import { CategoryRow } from './components/Row/CategoryRow';
import { BannerRow } from './components/Row/BannerRow';

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
            {/* Linha de Banners totalmente dinâmica consumindo a API e o TenantId */}
            <BannerRow />

            <CategoryRow />

            <div className="flex justify-between items-end mb-4 mt-2">
                <h2 className="text-xl font-black text-[#0f172a]">Ofertas em Destaque</h2>
                <a href="#" className="text-blue-600 font-bold text-[13px] hover:underline">Ver mais &gt;</a>
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