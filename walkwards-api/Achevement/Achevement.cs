using walkwards_api.Sql;

namespace walkwards_api.Achevement;

public class Achevement
{
    public  AchevementType Type;
    public string Name;
    public object Obj;

    public Achevement(AchevementType type, string name, int obj)
    {
        Type = type;
        Name = name;
        Obj = GetProperties(obj, type).Result; 
    }

    private static async Task<object> GetProperties(int objId, AchevementType objType)
    {
        List<Dictionary<string, dynamic>> data = new List<Dictionary<string, dynamic>>();
        switch (objType)
        {
            case AchevementType.Challenge:
                data = await SqlManager.Reader($"SELECT * FROM base.challenge WHERE id = {objId};");
                return data[0];
            default:
                return null;
        }
    }

    public async Task AddAchevement(int uid)
    {
        
    }
}