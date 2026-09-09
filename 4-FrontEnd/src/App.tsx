import { AsaasSettings } from './pages/admin/AsaasSettings';
import { FinancePayments } from './pages/admin/FinancePayments';
import { SessionProvider, AdminOnly } from './components/SessionProvider';
import { CartPage } from './pages/CartPage';
import { CheckoutPage } from './pages/CheckoutPage';
import { AccountPage } from './pages/AccountPage';
import { OrdersPage } from './pages/OrdersPage';
import { ShippingSettings } from './pages/admin/ShippingSettings';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import { StoreHome } from './pages/StoreHome';
import { ProductDetail } from './pages/ProductDetail';
import { AdminLogin } from './pages/admin/AdminLogin';
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
            <SessionProvider><Routes>
                <Route path="/admin/asaas" element={<AdminOnly><AsaasSettings /></AdminOnly>} />
                <Route path="/admin/payments" element={<AdminOnly><FinancePayments /></AdminOnly>} />
                {/* Rotas públicas da Loja Virtual */}
                <Route path="/" element={<StoreHome />} />
                <Route path="/product/:id" element={<ProductDetail />} />
                <Route path="/cart" element={<CartPage />} />
                <Route path="/checkout" element={<CheckoutPage />} />
                <Route path="/account" element={<AccountPage />} />
                <Route path="/orders" element={<OrdersPage />} />
                <Route path="/orders/:id" element={<OrdersPage />} />
                <Route path="/admin/shipping" element={<AdminOnly><ShippingSettings /></AdminOnly>} />
                <Route path="*" element={<main className="store-main"><h1>Página não encontrada</h1><Link to="/">Voltar à loja</Link></main>} />

                {/* Rota de Acesso ao Painel Administrativo / Backoffice */}
                <Route path="/admin/login" element={<AdminLogin />} />

                {/* Auto-cadastro público de uma nova empresa (Tenant) na plataforma */}
                <Route path="/admin/register" element={<main className="store-main"><h1>Cadastro de novas lojas</h1><p>O provisionamento de lojas é realizado pela administração da plataforma nesta etapa.</p><Link to="/admin/login">Voltar ao acesso</Link></main>} />

                {/* Painel administrativo (Backoffice) */}
                <Route path="/admin/dashboard" element={<AdminOnly><Dashboard /></AdminOnly>} />
                <Route path="/admin/departments" element={<AdminOnly><Departments /></AdminOnly>} />
                <Route path="/admin/banners" element={<AdminOnly><StoreBanners /></AdminOnly>} />
                <Route path="/admin/order-sectors" element={<AdminOnly><OrderSectors /></AdminOnly>} />
                <Route path="/admin/order-statuses" element={<AdminOnly><OrderStatuses /></AdminOnly>} />
                <Route path="/admin/customers" element={<AdminOnly><Customers /></AdminOnly>} />
                <Route path="/admin/products" element={<AdminOnly><Products /></AdminOnly>} />
                <Route path="/admin/orders" element={<AdminOnly><OrdersKanban /></AdminOnly>} />
            </Routes></SessionProvider>
        </BrowserRouter>
    );
}

export default App;
