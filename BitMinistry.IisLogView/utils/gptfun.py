from typing import List
import openai
from _config import *
from decimal import Decimal
import json,re 
from google import genai
from google.genai import types

def ask_gemini(
    command: str,
    user_input: str,
    api_key: str = GEMINI_KEY,
    model: str = "gemini-2.5-flash",
    temperature: float = 1.0
):
    client = genai.Client(api_key=api_key)
    
    tools = [types.Tool(google_search=types.GoogleSearch())]
    
    # REMOVED response_mime_type="application/json"
    config = types.GenerateContentConfig(
        tools=tools,
        temperature=temperature
    )

    full_prompt = f"System Instructions: {command}\n\nUser Input: {user_input}"
    
    try:
        response = client.models.generate_content(
            model=model,
            contents=full_prompt,
            config=config
        )
        
        # Clean Markdown formatting if the model adds it
        text_output = response.text
        json_match = re.search(r'```json\s*(.*?)\s*```', text_output, re.DOTALL)
        if json_match:
            return json_match.group(1)
        return text_output
        
    except Exception as e:
        print(f"Gemini API Error: {e}")
        return None


def ask_gpt(
    command: str,
    input: str,
    temperature= 0.1,
    model=GPT_MODEL_3_5,
    max_tokens=16384,
    gptClient: openai.OpenAI = None
):
    if not isinstance(input, str):
        input = json.dumps(input, ensure_ascii=False, indent=2, default=str)
    if not isinstance(command, str):
        command = json.dumps(command, ensure_ascii=False, indent=2, default=str)

    messages = [
        {"role": "system", "content": command},
        {"role": "user", "content": input}
    ]
    
    if gptClient is None:
        gptClient = openai.OpenAI(api_key=GPT_KEY)
    try:
        response = gptClient.chat.completions.create(
            model=model,
            messages=messages,
            temperature=temperature,
            max_tokens=max_tokens,
            response_format={"type": "json_object"},
        )
        content = response.choices[0].message.content
        return content
    except Exception as e:
        print(e)
        return None





def gpt_questions(
    command: str,
    inputs: List[str],
    temperature: Decimal,
    model=GPT_MODEL_3_5,
    apikey = GPT_KEY,
    baseurl = "https://api.openai.com/v1", 
    max_tokens=16384
):
    if not isinstance(inputs, list) or not all(isinstance(x, str) for x in inputs):
        raise TypeError("inputs must be a list of strings.")

    xclient = openai.OpenAI(api_key=apikey, base_url= baseurl)
    gptanswers = []
    for up in inputs:
        if baseurl == XAIAPI_BASEURL:
            answ = ask_xai(
                command=command,
                input=up,
                temperature=temperature,
                model=model,
                max_tokens=max_tokens,
                gptClient=xclient
            )            
        else :
            answ = ask_gpt(
                command=command,
                input=up,
                temperature=temperature,
                model=model,
                max_tokens=max_tokens,
                gptClient=xclient
            )
        gptanswers.append(answ)
    return gptanswers






def ask_xai(
    command: str,
    input: str,
    temperature: float = 0.1,
    model: str = "grok-code-fast-1",  # Use actual xAI model name
    max_tokens: int = 4096,  # Lower default to a safer value
    gpt_client: openai.OpenAI = None
) -> dict | None:

    # Convert non-string inputs to JSON strings
    if not isinstance(command, str):
        try:
            command = json.dumps(command, ensure_ascii=False, indent=2, default=str)
        except TypeError as e:
            print(f"Error converting command to JSON: {e}")
            return None
    if not isinstance(input, str):
        try:
            input = json.dumps(input, ensure_ascii=False, indent=2, default=str)
        except TypeError as e:
            print(f"Error converting input to JSON: {e}")
            return None

    # Define messages for the API request
    messages = [
        {"role": "system", "content": command},
        {"role": "user", "content": input}
    ]

    # Initialize OpenAI client for xAI API if none provided
    if gpt_client is None:
        try:
            gpt_client = openai.OpenAI(
                api_key=XAIAPI_KEY, 
                base_url="https://api.x.ai/v1"   # xAI API endpoint
            )
        except Exception as e:
            print(f"Error initializing xAI client: {e}")
            return None

    # Send request to xAI API
    try:
        response = gpt_client.chat.completions.create(
            model=model,
            messages=messages,
            temperature=temperature,
            max_tokens=max_tokens,
            response_format={"type": "json_object"}
        )
        content = response.choices[0].message.content
        
        return content        

    except openai.APIError as e:
        print(f"xAI API error: {e}")
        return None
    except Exception as e:
        print(f"Unexpected error: {e}")
        return None