import { Modal } from '@nextui-org/react'
import Login from './Login'
import Register from './Register'

export default (props) => {
    return (
        <Modal
            isDismissable={false}
            isOpen={props.modal.isOpen}
            onOpenChange={props.modal.onOpenChange}
            backdrop="opaque"
            size={props.isRegisterModal ? "5xl" : "xl"}
            placement="center"
            className={`bg-white dark:bg-black p-0 ${props.isRegisterModal ? 'sm:h-5/6' : ''}`}
            hideCloseButton={true}
            classNames={{
                backdrop: "bg-gradient-to-t from-zinc-900 to-zinc-900/10 backdrop-opacity-20"
            }}
        >
            {props.isRegisterModal
                ? <Register onLogin={props.onLogin} modal={props.modal} changeModal={props.changeModal} isDarkMode={props.isDarkMode} />
                : <Login onLogin={props.onLogin} modal={props.modal} changeModal={props.changeModal} isDarkMode={props.isDarkMode} />
            }
        </Modal>
    );
};
