using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsRemoteControlService
{
    public delegate void OnActionCallback(Guid guid, string result, bool canClose);

    public interface IActionPlugin
    {
        /// <summary>
        /// Инициализирует плагин
        /// </summary>
        /// <param name="guid">Уникальное id плагина</param>>
        /// <param name="parameters">Параметры инициализации</param>
        void Initialize(Guid guid, string parameters);

        event OnActionCallback OnActionCallback;
    }

    /// <summary>
    /// Происходит, когда плагин получает сообщение
    /// </summary>
    /// <param name="guid">Уникальное id плагина</param>
    /// <param name="user">От какого пользователя поступило сообщение</param>
    /// <param name="plugin">В какой плагин поступил метод</param>
    /// <param name="method">Метод</param>
    /// <param name="parameters">Параметры метода</param>
    public delegate void OnMessageReceive(Guid guid, string user, string plugin, string method, string parameters);

    public interface IRemotePlugin
    {
        /// <summary>
        /// Инициализирует плагин
        /// </summary>
        /// <param name="guid">Уникальное id плагина</param>
        /// <param name="parameters">Параметры инициализации</param>
        void Initialize(Guid guid, string parameters);

        /// <summary>
        /// Происходит, когда плагину нужно отправить сообщение
        /// </summary>
        /// <param name="toUser">Какой пользователь инициировал действие</param>
        /// <param name="message">Результат</param>
        void SendMessage(string toUser, string message);

        event OnMessageReceive OnMessageReceive;
    }

    public interface IModuleMetadata
    {
        string Name { get; }
    }
}
