# Teaching LLMs New Tricks: Build Your Own MCP Skills

## 1. Opening / Hook
- What problem are we solving? LLMs are powerful but "frozen" at training time
- Real-world scenarios where out-of-the-box LLMs fall short (no live data, no tool access, no domain knowledge)
- Brief demo or teaser of what's possible with MCP

## 2. What is MCP?
- Model Context Protocol — what it is and why it was created
- How it standardizes the way LLMs interact with external tools and data
- MCP vs. other approaches (function calling, RAG, fine-tuning) — when to use what
- The ecosystem: who's adopting it (Anthropic, Claude, etc.)

## 3. The MCP Standard
- MCP is an open standard — who maintains it and where to find the spec
- Support for Resources and Prompts may be lacking
### The Three MCP Primitives
#### 1. Tools — *Things the LLM can DO*
- Actions or functions the LLM can invoke on demand
- Think of these as callable endpoints: query a database, send an email,
  look up a record, perform a calculation
- The LLM decides when to call a tool based on the tool's name and description
- Tools accept typed input parameters and return a result back to the LLM
- This is the most commonly used primitive and likely the focus of your demo
- Example: A tool called `get_customer_orders` that takes a customer ID
  and returns their order history

#### 2. Resources — *Things the LLM can READ*
- Static or dynamic data that the LLM can access as context
- Unlike tools, resources are not "called" — they are read, similar to
  opening a file or loading a document
- Can be static (a markdown file, a config doc) or dynamic
  (a live database record, a URL's contents)
- Useful for giving the LLM background knowledge, reference data,
  or domain-specific content it wasn't trained on
- Example: A resource that exposes your product catalog or internal
  knowledge base articles

#### 3. Prompts — *Things the LLM can USE as templates*
- Pre-defined, reusable prompt templates stored on the MCP server
- Allow developers to standardize how the LLM is instructed for
  specific tasks — keeping prompt logic on the server, not hardcoded
  in the client
- Can accept parameters to make them dynamic
- Useful for enforcing consistency across interactions
  (e.g., always summarize support tickets the same way)
- Example: A prompt template called `summarize_ticket` that structures
  how the LLM should analyze and summarize a support request
- What makes a valid MCP server — required vs. optional capabilities
- The transport layer: stdio vs. HTTP/SSE
- How capability negotiation works (the handshake between client and server)
- Versioning in the spec — how compatibility is managed

## 4. MCP Architecture
- Core concepts: MCP Host, MCP Client, MCP Server
- What a "tool" looks like from the LLM's perspective
- How the LLM decides when and how to call a tool
- Request/response flow walkthrough

## 5. Building Your First MCP Server
- Choosing a language/framework (great opportunity to highlight .NET options)
- Defining tools — naming, descriptions, and parameter schemas matter a lot
- How the LLM reads your tool descriptions to decide when to invoke them
- Returning structured responses back to the LLM

## 6. Live Demo — Your Sample App
- Walk through the MCP server you built
- Show the LLM invoking tools in real time
- Highlight the before/after — what the LLM couldn't do vs. what it can do now

## 7. Best Practices
- Write clear, descriptive tool names and descriptions (the LLM relies on these!)
- Keep tools focused and single-purpose
- Handling errors gracefully and returning useful feedback to the LLM
- Security considerations — don't expose things the LLM shouldn't control
- Prompt injection risks — malicious content trying to hijack tool calls

## 8. Integration Scenarios
- Connecting to databases or internal APIs
- Wrapping existing business logic
- Multi-server setups — composing multiple MCP serve[text](../../../Downloads/mcp-talk_3.html)rs together
- Local vs. hosted MCP servers

## 9. Practical Considerations
- Debugging MCP interactions
- Show the MCP Inspector tool
- Testing your MCP server independently
- Versioning and evolving your tools over time

## 10. Where is MCP Headed?
- Growing ecosystem of pre-built MCP servers
- Standardization and tooling improvements
- Opportunities for the .NET community to contribute

## 11. Q&A / Resources
- MCP spec and official docs
- .NET SDK / libraries for building MCP servers
- Your sample app repo (if sharing)
- Where to go next