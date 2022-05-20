using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.IdCryptConnectionMessageHandler.TestData;

internal static class SendProofRequest
{
	public const string Path = "/present-proof/send-request";

	private static string SerialisedResponse => @"
{
   ""auto_present"":false,
   ""by_format"":{
      ""pres"":{       
      },
      ""pres_proposal"":{         
      },
      ""pres_request"":{         
      }
   },
   ""connection_id"":""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
   ""created_at"":""2021-12-31 23:59:59Z"",
   ""error_msg"":""Invalid structure"",
   ""initiator"":""self"",
   ""pres"":{
      ""@id"":""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
      ""@type"":""https://didcomm.org/my-family/1.0/my-message-type"",
      ""comment"":""string"",
      ""formats"":[
         {
            ""attach_id"":""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""format"":""dif/presentation-exchange/submission@v1.0""
		 }
      ],
      ""presentations~attach"":[
         {
            ""@id"":""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""byte_count"":1234,
            ""data"":{
               ""base64"":""ey4uLn0="",
               ""json"":""{\""sample\"": \""content\""}"",
               ""jws"":{
                  ""header"":{
                     ""kid"":""did:sov:LjgpST2rjsoxYegQDRm7EL#keys-4""
                  },
                  ""protected"":""ey4uLn0"",
                  ""signature"":""ey4uLn0"",
                  ""signatures"":[
                     {
                        ""header"":{
                           ""kid"":""did:sov:LjgpST2rjsoxYegQDRm7EL#keys-4""
                        },
                        ""protected"":""ey4uLn0"",
                        ""signature"":""ey4uLn0""
                     }
                  ]
               },
               ""links"":[
                  ""https://link.to/data""
               ],
               ""sha256"":""617a48c7c8afe0521efdc03e5bb0ad9e655893e6b4b51f0e794d70fba132aacb""
            },
            ""description"":""view from doorway, facing east, with lights off"",
            ""filename"":""IMG1092348.png"",
            ""lastmod_time"":""2021-12-31 23:59:59Z"",
            ""mime-type"":""image/png""
         }
      ]
   },
   ""pres_ex_id"":""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
   ""pres_proposal"":{
	""@id"":""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
      ""@type"":""https://didcomm.org/my-family/1.0/my-message-type"",
      ""comment"":""string"",
      ""formats"":[
		 {
			""attach_id"":""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""format"":""dif/presentation-exchange/submission@v1.0""
		 }
      ],
      ""proposals~attach"":[
		 {
		""@id"":""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""byte_count"":1234,
            ""data"":{
			""base64"":""ey4uLn0="",
               ""json"":""{\""sample\"": \""content\""}"",
               ""jws"":{
				""header"":{
					""kid"":""did:sov:LjgpST2rjsoxYegQDRm7EL#keys-4""
				  },
                  ""protected"":""ey4uLn0"",
                  ""signature"":""ey4uLn0"",
                  ""signatures"":[
					 {
					""header"":{
						""kid"":""did:sov:LjgpST2rjsoxYegQDRm7EL#keys-4""
						},
                        ""protected"":""ey4uLn0"",
                        ""signature"":""ey4uLn0""
					 }
                  ]
               },
               ""links"":[
				  ""https://link.to/data""
               ],
               ""sha256"":""617a48c7c8afe0521efdc03e5bb0ad9e655893e6b4b51f0e794d70fba132aacb""
			},
            ""description"":""view from doorway, facing east, with lights off"",
            ""filename"":""IMG1092348.png"",
            ""lastmod_time"":""2021-12-31 23:59:59Z"",
            ""mime-type"":""image/png""
		 }
      ]
   },
   ""pres_request"":{
	""@id"":""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
      ""@type"":""https://didcomm.org/my-family/1.0/my-message-type"",
      ""comment"":""string"",
      ""formats"":[
		 {
			""attach_id"":""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""format"":""dif/presentation-exchange/submission@v1.0""
		 }
      ],
      ""request_presentations~attach"":[
		 {
		""@id"":""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""byte_count"":1234,
            ""data"":{
			""base64"":""ey4uLn0="",
               ""json"":""{\""sample\"": \""content\""}"",
               ""jws"":{
				""header"":{
					""kid"":""did:sov:LjgpST2rjsoxYegQDRm7EL#keys-4""
				  },
                  ""protected"":""ey4uLn0"",
                  ""signature"":""ey4uLn0"",
                  ""signatures"":[
					 {
					""header"":{
						""kid"":""did:sov:LjgpST2rjsoxYegQDRm7EL#keys-4""
						},
                        ""protected"":""ey4uLn0"",
                        ""signature"":""ey4uLn0""
					 }
                  ]
               },
               ""links"":[
				  ""https://link.to/data""
               ],
               ""sha256"":""617a48c7c8afe0521efdc03e5bb0ad9e655893e6b4b51f0e794d70fba132aacb""
			},
            ""description"":""view from doorway, facing east, with lights off"",
            ""filename"":""IMG1092348.png"",
            ""lastmod_time"":""2021-12-31 23:59:59Z"",
            ""mime-type"":""image/png""
		 }
      ],
      ""will_confirm"":true
   },
   ""role"":""prover"",
   ""state"":""proposal-sent"",
   ""thread_id"":""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
   ""trace"":true,
   ""updated_at"":""2021-12-31 23:59:59Z"",
   ""verified"":""true""
}";

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, SerialisedResponse);
}
