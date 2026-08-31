import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { StoreHome } from './pages/StoreHome';
import { ProductDetail } from './pages/ProductDetail';
import { AdminLogin } from './pages/admin/AdminLogin';
import { RegisterCompany } from './pages/admin/RegisterCompany';
import { Dashboard } from './pages/admin/Dashboard';

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
            </Routes>
        </BrowserRouter>
    );
}

export default App;
