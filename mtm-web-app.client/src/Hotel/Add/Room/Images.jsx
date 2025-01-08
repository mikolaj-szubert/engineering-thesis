import React from 'react';
import { Button, Image, Input } from '@nextui-org/react'
import { toast } from 'react-toastify'

export default ({ data, setData }) => {
    const fileInputRef = React.useRef(null);

    const imgClickHandler = () => {
        fileInputRef.current.click();
    };

    const handleFileChange = (event) => {
        const files = Array.from(event.target.files);
        const validFiles = files.filter(file => file.type === 'image/png' || file.type === 'image/jpeg');
        const existingFiles = new Set(data.images.map(file => file.name + file.size));

        const uniqueFiles = validFiles.filter(file => {
            const fileKey = file.name + file.size;
            if (existingFiles.has(fileKey)) {
                toast.warning(`Plik ${file.name} został już dodany.`);
                return false;
            }
            existingFiles.add(fileKey);
            return true;
        });

        if (validFiles.length !== files.length) {
            if (files.length === 1) toast.error('Nie dodano pliku, ponieważ nie jest w formacie PNG lub JPG.');
            else toast.error('Niektóre pliki zostały pominięte, ponieważ nie są w formacie PNG lub JPG.');
        }

        if (uniqueFiles.length > 0) {
            setData(prev => ({
                ...prev,
                images: [...prev.images, ...uniqueFiles],
            }));
        }
    };

    const handleImageRemove = (image) => {
        setData(prev => ({
            ...prev,
            images: prev.images.filter(i => i !== image),
        }));
    };

    return (
        <div className="w-full px-2 sm:px-12 md:px-36 lg:px-64 xl:px-96 pb-24 mt-8">
            <div className="flex flex-wrap gap-4 justify-start items-center">
                {data.images.map((image, index) => (
                    <div key={index} className="relative">
                        <div className="w-[200px] h-[200px] overflow-hidden relative">
                            <img
                                src={URL.createObjectURL(image)}
                                alt={`Uploaded image ${index + 1}`}
                                className="h-full w-full object-contain object-center rounded-xl"
                            />
                        </div>
                        <Button
                            radius="full"
                            isIconOnly
                            variant="bordered"
                            className="absolute border-2 rounded-full border-black text-red-500 bg-black dark:bg-white z-50 bottom-2 right-2 translate-x-1/4 translate-y-1/4"
                            onClick={() => handleImageRemove(image)}
                        >
                            ✖
                        </Button>
                    </div>
                ))}
                <div>
                    <Image
                        onClick={imgClickHandler}
                        className="cursor-pointer"
                        width={200}
                        alt="Add hotel image"
                        src="/add-image.png"
                    />
                </div>
            </div>
            <Input
                type="file"
                accept="image/*"
                ref={fileInputRef}
                className="hidden"
                onChange={handleFileChange}
                multiple
            />
        </div>
    );
};
