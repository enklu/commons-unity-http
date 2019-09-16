using System;
using System.Collections;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// Provides an interface for bootstrapping coroutines.
    /// </summary>
    public interface IBootstrapper
    {
        /// <summary>
        /// Starts a coroutine.
        /// </summary>
        /// <param name="coroutine">The coroutine to start.</param>
        void BootstrapCoroutine(IEnumerator coroutine);

        /// <summary>
        /// Waits and then executes a callback.
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds to wait.</param>
        /// <param name="callback">The callback to execute.</param>
        void Wait(float milliseconds, Action callback);
    }
}