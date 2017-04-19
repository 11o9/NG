using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;
namespace NG
{
    [System.Runtime.InteropServices.Guid("116d72b5-0b7a-4f6c-bea2-a4b6f9123520")]
    public class NGCommand : Command
    {
        public NGCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static NGCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "NGCommand"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            Guid panelID = PanelHost.PanelId;
            bool visible = Panels.IsPanelVisible(panelID);
            if (visible)
                Rhino.UI.Panels.ClosePanel(panelID);
            else
                Rhino.UI.Panels.OpenPanel(panelID);
            return Result.Success;
            // ---

        }
    }
}
