//using Autodesk.Windows;
//using System.Linq;

//namespace WARBIMPRO.Services
//{
//    public class RibbonHackService
//    {
//        public void MovePanelToModify()
//        {
//            var ribbon = ComponentManager.Ribbon;

//            var modifyTab = ribbon.Tabs.FirstOrDefault(t => t.Id == "Modify");
//            var myTab = ribbon.Tabs.FirstOrDefault(t => t.Id == "WARBIMPRO");

//            if (modifyTab == null || myTab == null) return;

//            var myPanel = myTab.Panels
//                .FirstOrDefault(p => p.Source.Title == "VISTAS"); // tu panel

//            if (myPanel == null) return;

//            // 🔥 remover de tu tab
//            myTab.Panels.Remove(myPanel);

//            // 🔥 agregar a Modify
//            modifyTab.Panels.Add(myPanel);
//        }
//    }
//}