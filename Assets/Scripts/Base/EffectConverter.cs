// EffectConverter.cs
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

// 用于反序列化 BaseEffect 抽象类
public class EffectConverter : JsonConverter<BaseEffect>
{
    public override BaseEffect ReadJson(JsonReader reader, Type objectType, BaseEffect existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // 从 JSON 中读取整个对象
        JObject jsonObject = JObject.Load(reader);

        // 假设 JSON 中包含一个 "actionType" 字段来指明具体的子类
        // 注意：这里需要你手动添加一个字段到你的 JSON 中，或者通过其他字段推断
        // 我们假设你会在 JSON 中添加一个 "type": "DamageEffect" 字段
        string typeName = jsonObject["type"]?.ToString();

        if (string.IsNullOrEmpty(typeName))
        {
            throw new JsonSerializationException("缺少 'type' 字段来确定 BaseEffect 的具体类型。");
        }

        BaseEffect effect = null;

        // --- 核心：根据 typeName 创建具体的子类实例 ---
        switch (typeName)
        {
            case nameof(DamageEffect):
                effect = new DamageEffect();
                break;
            case nameof(DrawCardEffect):
                effect = new DrawCardEffect();
                break;
            // --- 新增效果类型 ---
            case nameof(BuffUnitEffect):
                effect = new BuffUnitEffect();
                break;
            case nameof(BuffPlayerEffect):
                effect = new BuffPlayerEffect();
                break;
            case nameof(GainRuneEffect):
                effect = new GainRuneEffect();
                break;
            // ------------------
            case nameof(SummonEffect): // 保持之前的示例
                effect = new SummonEffect();
                break;
            default:
                throw new JsonSerializationException($"未知的效果类型: {typeName}");
        }

        serializer.Populate(jsonObject.CreateReader(), effect);
        return effect;
    }

    public override void WriteJson(JsonWriter writer, BaseEffect value, JsonSerializer serializer)
    {
        // 序列化时，将具体的子类类型名称也写入 JSON，以便 ReadJson 可以识别
        JObject jsonObject = JObject.FromObject(value, serializer);
        jsonObject.AddFirst(new JProperty("type", value.GetType().Name));
        jsonObject.WriteTo(writer);
    }
}