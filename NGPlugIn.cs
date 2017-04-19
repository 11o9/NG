using Rhino.PlugIns;
using Rhino.UI;
namespace NG
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class NGPlugIn : Rhino.PlugIns.PlugIn

    {
        public NGPlugIn()
        {
            Instance = this;
        }

        ///<summary>Gets the only instance of the NGPlugIn plug-in.</summary>
        public static NGPlugIn Instance
        {
            get; private set;
        }

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            System.Type panel_type = typeof(PanelHost);
            var b = panel_type.IsPublic;
            try
            {
                Panels.RegisterPanel(this, panel_type, "SampleWpf", Properties.Resources.Icon1);
            }
            catch (System.Exception e)
            {
                throw e;
            }
                return LoadReturnCode.Success;
        }

      
        // You can override methods here to change the plug-in behavior on
        // loading and shut down, add options pages to the Rhino _Option command
        // and mantain plug-in wide options in a document.
    }
    [System.Runtime.InteropServices.Guid("4FF5E249-AEE3-4F7B-8EFB-78CE8B358156")]
    public class PanelHost : RhinoWindows.Controls.WpfElementHost
    {
        public PanelHost()
          : base(new Panel(), null) // No view model (for this example)
        {
        }

        /// <summary>
        /// Returns the ID of this panel.
        /// </summary>
        public static System.Guid PanelId
        {
            get
            {
                return typeof(PanelHost).GUID;
            }
        }
    }
}