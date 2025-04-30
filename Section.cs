using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Убедитесь, что в проекте нет дублирующихся определений класса Section.  
// Если класс Section уже определён в другом файле, удалите одно из определений.  
// Если это не так, проверьте, не подключён ли файл дважды в проекте.  

// Если проблема сохраняется, переименуйте класс, чтобы избежать конфликта:  
namespace WpfLb1
{
    public class Section
    {
        public string Name { get; set; }
        public List<Subsection> Subsections { get; set; } = new List<Subsection>();
    }

    public class Subsection
    {
        public string Name { get; set; }
        public List<Topic> Topics { get; set; } = new List<Topic>();
    }

    
}
