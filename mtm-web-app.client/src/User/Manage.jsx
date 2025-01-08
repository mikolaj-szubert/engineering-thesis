import { ProtectedRoute } from '../Helpers';
import { useOutletContext } from 'react-router-dom';

export default () => {
    const { user, loggingWrapperForm } = useOutletContext();
    return (
        <ProtectedRoute user={user} loggingWrapperForm={loggingWrapperForm} >
            <h1>Hello World!</h1>
        </ProtectedRoute>
    );
}