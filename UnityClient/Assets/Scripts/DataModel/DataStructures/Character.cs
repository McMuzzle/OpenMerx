﻿// MIT License

// Copyright(c) 2017 Andre Plourde
// part of https://github.com/BorealianStudio/OpenMerx

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.


// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;

public class Character : DataObject {
    public int ID { get; private set; }
    public string Name { get; set; }
    public int Corp { get; set; }

    public Character(int id) {
        ID = id;
        Corp = -1;
    }

    public override List<EventHolder> Update(DataObject o) {
        List<EventHolder> result = new List<EventHolder>();
        Character c = o as Character;
        if (null == c)
            return result;

        string data = Newtonsoft.Json.JsonConvert.SerializeObject(c);
        var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings { ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace };
        Newtonsoft.Json.JsonConvert.PopulateObject(data, this, serializerSettings);

        result.Add(new CharacterChangeEvent(this, LocalDataManager.instance.OnCharacterChange));
        
        return result;
    }
}
