using JetBrains.Annotations;

namespace WookiepediaStatusArticleData.Nominations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class NominatorAttribute
{
    public int Id { get; set; }
    
    public int NominatorId { get; set; }
    
    public Nominator? Nominator { get; set; }
    
    public required NominatorAttributeType AttributeName { get; set; }
    
    public required DateTime EffectiveAt { get; set; }
    
    public DateTime? EffectiveEndAt { get; set; }
}