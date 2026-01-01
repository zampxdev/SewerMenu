namespace SewerMenu.UI.Pages
{
    /// <summary>
    /// Interface for menu pages.
    /// </summary>
    public interface IPage
    {
        /// <summary>
        /// Gets the page title.
        /// </summary>
        string Title { get; }
        
        /// <summary>
        /// Initializes the page.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Draws the page content.
        /// </summary>
        void Draw();
        
        /// <summary>
        /// Shuts down the page.
        /// </summary>
        void Shutdown();
    }
}
