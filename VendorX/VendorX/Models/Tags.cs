using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vendor.Models
{
    public  class Tags:INotifyPropertyChanged
    {
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        private string _name = string.Empty;
        public string NameTag { get { return _name; } set { _name = value; OnPropertyChanged(nameof(NameTag)); } }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        public Tags()
        {

        }
    }
}
