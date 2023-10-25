namespace LdapQueryParserTest;

public class LdapQueryBuilderTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("description=*pwd*", "(description=*pwd*)")]
    [InlineData("description=*pass* OR description=*pwd*", "(|(description=*pass*)(description=*pwd*))")]
    [InlineData("(description=*pass* OR description=*pwd*) AND (description=*pass* OR description=*pwd*)", "(&(|(description=*pass*)(description=*pwd*))(|(description=*pass*)(description=*pwd*)))")]
    [InlineData("(description=*pass* OR description=*pwd*) AND (description=*pass* OR description=*pwd*)", "(&(|(description=*pass*)(description=*pwd*))(|(description=*pass*)(description=*pwd*)))")]
    [InlineData("objectClass=user AND servicePrincipalName=* AND cn!=krbtgt AND userAccountControl:1.2.840.113556.1.4.803:!=2", "(&(objectClass=user)(servicePrincipalName=*)(!(cn=krbtgt))(!(userAccountControl:1.2.840.113556.1.4.803:=2)))")]
    public void Create_ForAGivenQuery_ShouldCreateCorrectLdapQuery(string query, string expected)
    {
        var ldapQuery = LdapQueryBuilder.Create(query);
        Assert.Equal(expected, ldapQuery);
    }
}
