## LDAP Query Builder from SQL like syntax

### Objective
This library helps in creating LDAP queries using natural SQL like syntax
See the examples below

```csharp
namespace LdapQueryParser;
public class Program
{
    public static void Main()
    {
            var queries = new List<string>
            {
                "description!=pass*",
                "description=*pwd*",
                "description=*pass* OR description=*pwd*",
                "(description=*pass* OR description=*pwd*) AND (description=*pass* OR description=*pwd*)",
                "objectCategory=user AND (description=*pass* OR description=*pwd*)",
                "objectCategory=user AND (description=*pass* OR description=*pwd*)",
                "(description=*pass* OR (description=*pass* OR description=*pwd*)) AND (description=*pass* OR description=*pwd*)",
                "objectClass=user AND servicePrincipalName=* AND cn!=krbtgt AND userAccountControl:1.2.840.113556.1.4.803:!=2",
                "objectCategory=1 AND ((description=2 OR description=3) AND (description=4 OR (description=5 AND description=6))) AND (description=7 OR description=8)",
            };
    
            foreach (var query in queries)
            {
                Console.WriteLine(query);
                Console.WriteLine(LdapQueryBuilder.Create(query));
                Console.WriteLine("----------------------------------------");
            }
    }
}
```
