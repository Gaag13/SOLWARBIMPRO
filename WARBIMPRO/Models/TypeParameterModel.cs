using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WARBIMPRO.Models
{   
    public class TypeParameterModel
    {
        public string Name { get; set; }
        public StorageType StorageType { get; set; }
        public object Value { get; set; }
        public bool IsEditable { get; set; }
    }
    
    
    
}
