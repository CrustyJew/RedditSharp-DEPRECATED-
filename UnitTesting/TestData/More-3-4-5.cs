using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTesting.TestData
{
	public static class More_3_4_5
	{
		public static string GetMore_3_4_5()
		{
			return @"
{
	""json"": {
		""errors"": [],
		""data"": {
			""things"": [
				{
					""kind"": ""t1"",
					""data"": {
						""id"": ""3"",
						""name"": ""t1_3"",
						""parent_id"": ""t3_post""
					}
},
				{
					""kind"": ""t1"",
					""data"": {
						""id"": ""3-1"",
						""name"": ""t1_3-1"",
						""parent_id"": ""t1_3""
					}
				},
				{
					""kind"": ""more"",
					""data"": {
						""id"": ""3-more"",
						""name"": ""t1_3-more"",
						""children"": [ ""3-2"" ],
						""parent_id"": ""t1_3""
					}
				},
				{
					""kind"": ""t1"",
					""data"": {
						""id"": ""4"",
						""name"": ""t1_4"",
						""parent_id"": ""t3_post""
					}
				},
				{
					""kind"": ""t1"",
					""data"": {
						""id"": ""5"",
						""name"": ""t1_5"",
						""parent_id"": ""t3_post""
					}
				},
				{
					""kind"": ""t1"",
					""data"": {
						""id"": ""5-1"",
						""name"": ""t1_5-1"", ""parent_id"": ""t1_5""
					}
				},
				{
					""kind"": ""t1"",
					""data"": {""id"": ""5-1-1"",""name"": ""t1_5-1-1"",""parent_id"": ""t1_5-1""}
				},
				{
					""kind"": ""more"",
					""data"": {""id"": ""5-1-more"", ""name"": ""5-1-more"",""parent_id"": ""t1_5-1"" ,""children"": [""5-1-2""]}
				}

			]
		}
	}
}
";
		}
	}
}
