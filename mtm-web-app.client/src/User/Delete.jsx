import React from 'react'
import { useOutletContext } from 'react-router-dom'
import { instance } from '../Helpers'
import { translations } from '../lang'
import { Button } from '@nextui-org/react'

export default () => {
    const [isLoading, setIsLoading] = React.useState(false);
    const { onLogout } = useOutletContext();
    const deleteBtnClick = () => {
        instance.delete('account/manage/delete')
            .then((res) => {
                if (res.status === 200) {
                    onLogout();
                }
                else {
                    console.error(res.data);
                }
            })
            .catch(err=>console.error(err))
            .finally(()=>setIsLoading(false));
    }
    return (
        <div className="px-4 md:px-24 py-24">
            <Button
                isLoading={isLoading}
                onPress={deleteBtnClick}
                color="danger"
                className="w-full text-white text-lg font-medium"
                radius="full"
                variant="solid"
            >
                {translations.Delete}
            </Button>
        </div>
    );
}