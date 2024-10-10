namespace Passi.Core.Domain.Entities
{
    /// <summary>
    /// Definisce un filtro in una convenzione.
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// Tipo del filtro
        /// </summary>
        public string Type { get; internal set; } = string.Empty;

        /// <summary>
        /// Visibilità del filtro
        /// </summary>
        public string Scope { get; internal set; } = string.Empty;

        /// <summary>
        /// Valore del filtro
        /// </summary>
        public string Value { get; internal set; } = string.Empty;
    }
}
