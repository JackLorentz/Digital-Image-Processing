namespace DIP_HW
{
    internal class ComboboxItem
    {
        private int value;
        public int Value
        {
            get { return value; }
        }
        private string text;
        public string Text
        {
            get { return text; }
        }

        public ComboboxItem(int v1, string v2)
        {
            this.value = v1;
            this.text= v2;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}