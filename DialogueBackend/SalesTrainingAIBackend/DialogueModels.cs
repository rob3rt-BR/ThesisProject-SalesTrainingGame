namespace DialogueModels
{
    
    public sealed class DialogueRequest
    {
        public required string ScenarioId {get; set;}
        public required string ModuleId {get; set;}
        public required string QuestionId {get; set;}

        public required string CustomerPersona {get; set;}
        public required string ScenarioContext {get; set;}
        public required string QuestionDialogueContext {get; set;}

        public required string QuestionText {get; set;}
        public required string SelectedAnswer {get; set;}

        public List<DialogueMessage> ConversationHistory {get; set;} = [];
    }

    public sealed class DialogueMessage
    {
        public required string Role {get; set;}
        public required string Content {get; set;}
    }

    public sealed class DialogueResponse
    {
        public string ResponseId {get; set;} = "";
        public string Model {get; set;} = "";
        public string ReplyText {get; set;} = "";
    }

    public sealed class OpenRouterMessage
    {
        public string Role {get; set;} = "";
        public string Content {get; set;} = "";
    }

    public sealed class OpenRouterChoice
    {
        public OpenRouterMessage Message {get; set;} = new();
    }

    public sealed class OpenRouterResponse
    {
        public string Id {get; set;} = "";
        public string Model {get; set;} = "";

        public List<OpenRouterChoice> Choices {get; set;} = [];
    }
}
