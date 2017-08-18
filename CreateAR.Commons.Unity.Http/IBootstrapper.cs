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
    }
}