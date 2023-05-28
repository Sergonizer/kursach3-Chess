using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Chess
{
    public class ServerUser //класс пользователя
    {
        public int ID {get;set;} //Id
        public string Name { get;set;} // имя
        public OperationContext OperationContext { get;set;} //сведения о подключении пользователя
    }
}
