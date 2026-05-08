/// <summary>
/// IDatabaseAccess — Interface from UML.
/// Any class that talks to the database MUST implement these 3 methods.
/// This is the "contract" — it doesn't care HOW you save/fetch/delete,
/// just that you CAN do those three things.
/// 
/// UML: <<interface>> IDatabaseAccess
///   +save(data): void   (abstract)
///   +fetch(id): Object  (abstract)
///   +delete(id): void   (abstract)
/// </summary>
public interface IDatabaseAccess
{
    void Save(object data);
    object Fetch(string id);
    void Delete(string id);
}
