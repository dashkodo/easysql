using FluentAssertions;

namespace EasySql.Tests;

public class AutoCompleteTests
{
    private static string inputText = """
        select
            SampleTableId,
            1033, -- en-US
            ResourceId,
            @CreatedByUserId,
            @CreatedByUserId
        from SampleTable st
        join SampleTable1 ast on 
        """;
    
     
    public void ShouldKeepFormatting(string input)
    {
        new Formatter(input.TrimStart('\n')).Format().Should().Be(input);
    }
}