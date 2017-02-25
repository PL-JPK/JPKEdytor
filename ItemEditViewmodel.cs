using Puch.Common;


namespace Puch.JPK
{
    public class ItemEditViewmodel
    {
        public ViewmodelBase ItemContent { get; set; }
        public string WindowTitle { get; set; }
        public static bool? ShowModal(ViewmodelBase item, string windowTitle)
        {
            bool? result = new ItemEditView() {
                DataContext = 
                    new ItemEditViewmodel() { ItemContent = item, WindowTitle = windowTitle } }
                    .ShowDialog();
            if (result == true)
                item.SaveToModel();
            return result;
        }
    }
}
