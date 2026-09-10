import { AsaasSettings } from './pages/admin/AsaasSettings';
import { SecurityPage } from './pages/admin/SecurityPage';
import { AccessPage } from './pages/admin/AccessPage';
import { FinancePayments } from './pages/admin/FinancePayments';
import { SupportPage } from './pages/SupportPage';
import { FavoritesPage } from './pages/FavoritesPage';
import { NotificationsPage } from './pages/NotificationsPage';
import { CouponsPage } from './pages/admin/CouponsPage';
import { ImportsPage } from './pages/admin/ImportsPage';
import { CatalogDetailsPage } from './pages/admin/CatalogDetailsPage';
import { ReviewModeration } from './pages/admin/ReviewModeration';
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
import { OrderOperationsPage } from './pages/admin/OrderOperationsPage';

function App() {
    return (
        <BrowserRouter>
            <SessionProvider><Routes>
                <Route path="/admin/access" element={<AdminOnly><AccessPage/></AdminOnly>}/>
                <Route path="/admin/security" element={<AdminOnly><SecurityPage/></AdminOnly>}/>
                <Route path="/support" element={<SupportPage/>}/>
                <Route path="/favorites" element={<FavoritesPage/>}/>
                <Route path="/notifications" element={<NotificationsPage/>}/>
                <Route path="/admin/notifications" element={<AdminOnly><NotificationsPage admin/></AdminOnly>}/>
                <Route path="/admin/coupons" element={<AdminOnly><CouponsPage/></AdminOnly>}/>
                <Route path="/admin/imports" element={<AdminOnly><ImportsPage/></AdminOnly>}/>
                <Route path="/admin/catalog-details" element={<AdminOnly><CatalogDetailsPage/></AdminOnly>}/>
                <Route path="/admin/support" element={<AdminOnly><SupportPage admin/></AdminOnly>}/>
                <Route path="/admin/reviews" element={<AdminOnly><ReviewModeration/></AdminOnly>}/>
                <Route path="/admin/asaas" element={<AdminOnly><AsaasSettings /></AdminOnly>} />
                <Route path="/admin/payments" element={<AdminOnly><FinancePayments /></AdminOnly>} />
                {/* Rotas públicas da Loja Virtual */}
                <Route path="/" element={<StoreHome />} />
                <Route path="/product/:id" element={<ProductDetail />} />
                <Route path="/p/:slug" element={<ProductDetail />} />
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
                <Route path="/admin/orders" element={<AdminOnly><OrderOperationsPage /></AdminOnly>} />
            </Routes></SessionProvider>
        </BrowserRouter>
    );
}

export default App;
