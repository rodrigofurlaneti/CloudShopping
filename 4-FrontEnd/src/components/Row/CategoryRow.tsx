import { useEffect, useState } from 'react';
import { DepartmentService, type Department } from '../../services/api';
import {
    Monitor, Home, Shirt, Sparkles, Dumbbell,
    Puzzle, BookOpen, Gamepad2, PawPrint, CarFront, HelpCircle
} from 'lucide-react';

const iconMap: Record<string, any> = {
    'eletronicos': Monitor,
    'casa-e-cozinha': Home,
    'moda': Shirt,
    'beleza': Sparkles,
    'esportes': Dumbbell,
    'brinquedos': Puzzle,
    'livros': BookOpen,
    'games': Gamepad2,
    'pet-shop': PawPrint,
    'automotivo': CarFront
};

const mockupOrder = [
    'eletronicos',
    'casa-e-cozinha',
    'moda',
    'beleza',
    'esportes',
    'brinquedos',
    'livros',
    'games',
    'pet-shop',
    'automotivo'
];

export function CategoryRow() {
    const [departments, setDepartments] = useState<Department[]>([]);

    useEffect(() => {
        DepartmentService.getAll()
            .then(data => {
                const sortedData = [...data].sort((a, b) => {
                    const indexA = mockupOrder.indexOf(a.slug);
                    const indexB = mockupOrder.indexOf(b.slug);
                    return (indexA === -1 ? 999 : indexA) - (indexB === -1 ? 999 : indexB);
                });
                setDepartments(sortedData);
            })
            .catch(console.error);
    }, []);

    return (
        // Padding vertical reduzido drasticamente para py-2 para diminuir a altura pela metade
        <section className="bg-white rounded-2xl py-2 px-6 shadow-sm border border-gray-100 mb-4">
            <div className="flex justify-between items-center mb-1">
                <h2 className="text-[16px] font-extrabold text-[#0f172a]">Compre por categoria</h2>
                <a href="#" className="text-blue-600 text-[12px] font-bold flex items-center hover:underline">
                    Ver todas <span className="ml-0.5 text-base leading-none">&rsaquo;</span>
                </a>
            </div>

            <div className="flex justify-between items-center overflow-x-auto scrollbar-hide">
                {departments.map(dept => {
                    const Icon = iconMap[dept.slug] || HelpCircle;

                    return (
                        <div key={dept.id} className="flex flex-col items-center gap-1 min-w-[70px] cursor-pointer group py-1">
                            {/* Círculo compacto w-10 h-10 (40px) */}
                            <div className="w-10 h-10 rounded-full bg-[#f0f6ff] flex items-center justify-center text-blue-600 transition-all duration-300 group-hover:bg-blue-600 group-hover:text-white group-hover:shadow-sm">
                                <Icon size={18} strokeWidth={1.5} />
                            </div>
                            <span className="text-[11px] font-bold text-[#334155] text-center leading-tight group-hover:text-blue-600 transition-colors">
                                {dept.name}
                            </span>
                        </div>
                    );
                })}
            </div>
        </section>
    );
}