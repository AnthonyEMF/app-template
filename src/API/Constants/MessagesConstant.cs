namespace API.Constants
{
    public static class MessagesConstant
    {
        // Búsqueda
        public const string RECORDS_FOUND = "Registros encontrados correctamente";
        public const string RECORD_FOUND = "Registro encontrado correctamente";
        public const string RECORDS_NOT_FOUND = "Registros no encontrados";
        public const string RECORD_NOT_FOUND = "Registro no encontrado";

        // Creación
        public const string CREATE_SUCCESS = "Registro creado correctamente";
        public const string CREATE_ERROR = "Se produjo un error al crear el registro";

        // Actualización 
        public const string UPDATE_SUCCESS = "Registro actualizado correctamente";
        public const string UPDATE_ERROR = "Se produjo un error al actualizar el registro";

        // Eliminación 
        public const string DELETE_SUCCESS = "Registro eliminado correctamente";
        public const string DELETE_ERROR = "Se produjo un error al eliminar el registro";

        // Autenticación
        public const string LOGIN_SUCCESS = "Sesión iniciada correctamente";
        public const string LOGIN_ERROR = "Error al iniciar sesión";
        public const string REGISTRATION_SUCCESS = "Usuario registrado correctamente";
        public const string REGISTRATION_ERROR = "Error al registrar el usuario";
        public const string REFRESH_TOKEN_SUCCESS = "Sesión renovada correctamente";
        public const string REFRESH_TOKEN_ERROR = "Error al renovar la sesión";

        public const string TOKEN_EXPIRED = "La sesión ha expirado, por favor inicie sesión nuevamente";
        public const string INVALID_TOKEN = "La sesión no es válida, por favor inicie sesión nuevamente";
        public const string INVALID_EMAIL_CLAIM = "El token no contiene un claim de email válido";

        public const string WRONG_EMAIL = "Correo electrónico inválido, intente nuevamente";
        public const string WRONG_PASSWORD = "Contraseña incorrecta, intente nuevamente";
        public const string USER_NOT_FOUND = "El usuario ingresado no existe";
        public const string USER_BLOCKED = "El usuario se encuentra bloqueado, contacte a soporte técnico";

        public const string INVALID_USERNAME = "El nombre de usuario ingresado ya está en uso";
        public const string INVALID_EMAIL = "El correo electrónico ingresado ya está en uso";
        public const string INVALID_ROLE = "El rol seleccionado no existe";

        // Seeder
        public const string SEED_SUCCESS = "Datos de prueba cargados correctamente";
        public const string INVALID_ENV = "Este endpoint solo se puede ejecutar en entornos de desarrollo";


    }
}
