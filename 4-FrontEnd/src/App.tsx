import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { StoreHome } from './pages/StoreHome';
import { ProductDetail } from './pages/ProductDetail';
import { AdminLogin } from './pages/admin/AdminLogin';

function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Rotas públicas da Loja Virtual */}
                <Route path="/" element={<StoreHome />} />
                <Route path="/product/:id" element={<ProductDetail />} />

                {/* Rota de Acesso ao Painel Administrativo / Backoffice */}
                <Route path="/admin/login" element={<AdminLogin />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;