import { useEffect, useState } from 'react';
import { StoreBannerService, type StoreBanner } from '../../services/api';

export function BannerRow() {
    const [banners, setBanners] = useState<StoreBanner[]>([]);

    useEffect(() => {
        StoreBannerService.getAll()
            .then(data => setBanners(data))
            .catch(err => console.error("Erro ao buscar banners:", err));
    }, []);

    return (
        <div className="grid grid-cols-4 gap-4 mb-5">
            {banners.map((banner) => (
                <div
                    key={banner.id}
                    style={{ backgroundColor: banner.backgroundColor }}
                    className="h-[150px] rounded-2xl p-4 text-white shadow-sm relative overflow-hidden flex flex-col justify-between"
                >
                    <div>
                        <h2 className="text-lg font-extrabold leading-tight whitespace-pre-line">
                            {banner.title.replace(/ /g, '\n')}
                        </h2>

                        {banner.discountPercentage ? (
                            <div className="mt-1">
                                <span className="text-[10px] font-bold block mb-[-6px]">Até</span>
                                <div className="flex items-start">
                                    <span className="text-4xl font-black tracking-tighter">{banner.discountPercentage}</span>
                                    <div className="flex flex-col ml-1 mt-0.5">
                                        <span className="text-sm font-black leading-none">%</span>
                                        <span className="text-sm font-black leading-none">OFF</span>
                                    </div>
                                </div>
                            </div>
                        ) : (
                            <p className="text-[11px] mt-1 font-medium opacity-90 leading-snug">
                                {banner.subtitle}
                            </p>
                        )}
                    </div>

                    {/* Botão posicionado no canto inferior direito */}
                    <a
                        href={banner.buttonLink}
                        className="absolute bottom-4 right-4 bg-[#0f172a] text-white text-[11px] font-bold py-1.5 px-4 rounded-full z-10 hover:bg-gray-800 transition-colors shadow-md text-center"
                    >
                        {banner.buttonText}
                    </a>
                </div>
            ))}
        </div>
    );
}