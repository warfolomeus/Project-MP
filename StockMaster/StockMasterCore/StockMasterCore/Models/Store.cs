//Модель торговой точки (магазина). Отвечает за: Хранение информации о магазинах-клиентах, которые обслуживаются оптовым складом
namespace StockMasterCore.Models
{
    public class Store
    {
        public int Id { get; set; }                          // Уникальный идентификатор торговой точки
        public string Name { get; set; }                     // Название магазина/палатки
        public string Address { get; set; }                  // Адрес расположения торговой точки
        public string ContactPerson { get; set; }            // Контактное лицо для связи
    }
}