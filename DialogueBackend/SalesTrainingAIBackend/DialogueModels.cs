namespace DialogueModels
{
    
    public sealed class DialogueRequest
    {
        public string ScenarioId {get; set;} = "";
        public string ModuleId {get; set;} = "";
        public string QuestionId {get; set;} = "";

        public string CustomerPersona {get; set;} = "";
        public string ScenarioContext {get; set;} = "";
        public string QuestionDialogueContext {get; set;} = "";

        public string QuestionText {get; set;} = "";
        public string SelectedAnswer {get; set;} = "";

        public List<DialogueMessage> ConversationHistory {get; set;} = [];
    }

    public sealed class DialogueMessage
    {
        public string Role {get; set;} = "";
        public string Content {get; set;} = "";
    }

    public sealed class DialogueResponse
    {
        public string ResponseId {get; set;} = "";
        public string Model {get; set;} = "";
        public int PromptTokens {get; set;}
        public int CompletionTokens {get; set;}
        public long ResponseTimeMilliseconds {get; set;}

        public string TraineeDialogueText {get; set;} = "";
        public string ReplyText {get; set;} = "";
    }

    public sealed class DialogueOutput
    {
        public string TraineeDialogueText {get; set;} = "";
        public string ReplyText {get; set;} = "";
    }
}
