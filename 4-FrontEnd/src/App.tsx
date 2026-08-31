import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { StoreHome } from './pages/StoreHome';
import { ProductDetail } from './pages/ProductDetail';
import { AdminLogin } from './pages/admin/AdminLogin';
import { RegisterCompany } from './pages/admin/RegisterCompany';
import { Dashboard } from './pages/admin/Dashboard';
import { Departments } from './pages/admin/Departments';
import { StoreBanners } from './pages/admin/StoreBanners';
import { OrderSectors } from './pages/admin/OrderSectors';
import { OrderStatuses } from './pages/admin/OrderStatuses';
import { Customers } from './pages/admin/Customers';
import { Products } from './pages/admin/Products';
import { OrdersKanban } from './pages/admin/OrdersKanban';

function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Rotas públicas da Loja Virtual */}
                <Route path="/" element={<StoreHome />} />
                <Route path="/product/:id" element={<ProductDetail />} />

                {/* Rota de Acesso ao Painel Administrativo / Backoffice */}
                <Route path="/admin/login" element={<AdminLogin />} />

                {/* Auto-cadastro público de uma nova empresa (Tenant) na plataforma */}
                <Route path="/admin/register" element={<RegisterCompany />} />

                {/* Painel administrativo (Backoffice) */}
                <Route path="/admin/dashboard" element={<Dashboard />} />
                <Route path="/admin/departments" element={<Departments />} />
                <Route path="/admin/banners" element={<StoreBanners />} />
                <Route path="/admin/order-sectors" element={<OrderSectors />} />
                <Route path="/admin/order-statuses" element={<OrderStatuses />} />
                <Route path="/admin/customers" element={<Customers />} />
                <Route path="/admin/products" element={<Products />} />
                <Route path="/admin/orders" element={<OrdersKanban />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;
