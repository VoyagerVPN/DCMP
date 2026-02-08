```markdown
# SOLID Principles

SOLID is an acronym for five object-oriented design principles introduced by Robert C. Martin. These principles help create maintainable, scalable, and flexible code.

## Single Responsibility Principle (SRP)
A class should have only one reason to change — it should have a single, well-defined responsibility.

**TypeScript Example (Violation → Fix)**  
```typescript
// Violation: Parser handles both parsing and file writing
class HtmlParser {
  parse(html: string): string { /* extract data */ return data; }
  saveToFile(data: string, path: string) { /* write to disk */ }
}

// Fixed: Separate responsibilities
class HtmlParser {
  parse(html: string): string { /* extract data */ return data; }
}

class FileWriter {
  save(data: string, path: string) { /* write to disk */ }
}
```

## Open-Closed Principle (OCP)
Software entities should be open for extension but closed for modification.

**TypeScript Example**  
```typescript
interface ParserStrategy {
  parse(html: string): string;
}

class CheerioParser implements ParserStrategy { parse(html: string): string { /* ... */ } }
class JsDomParser implements ParserStrategy { parse(html: string): string { /* ... */ }

class SiteParser {
  constructor(private strategy: ParserStrategy) {}
  parse(html: string): string {
    return this.strategy.parse(html); // Extend by adding new strategies
  }
}
```

## Liskov Substitution Principle (LSP)
Subtypes must be substitutable for their base types without altering program correctness.

**JavaScript Example**  
```javascript
class BaseParser {
  parse(html) { /* common logic */ }
}

class ValidParser extends BaseParser {
  parse(html) { return super.parse(html); } // Works correctly
}

// Violation example: a subclass that throws unexpected errors or changes behavior
class InvalidParser extends BaseParser {
  parse(html) { throw new Error("Not supported"); } // Breaks substitution
}
```

## Interface Segregation Principle (ISP)
Clients should not be forced to depend on interfaces they do not use.

**TypeScript Example**  
```typescript
// Bad: Fat interface
interface Worker {
  parse(): void;
  save(): void;
  validate(): void;
}

// Good: Segregated interfaces
interface Parsable { parse(): void; }
interface Savable { save(): void; }

class Parser implements Parsable { parse() { /* ... */ } }
class Writer implements Savable { save() { /* ... */ }
```

## Dependency Inversion Principle (DIP)
High-level modules should depend on abstractions, not on concrete implementations.

**TypeScript Example**  
```typescript
interface DataExtractor {
  extract(html: string): any;
}

class SiteParser {
  constructor(private extractor: DataExtractor) {}
  parse(html: string) {
    return this.extractor.extract(html);
  }
}

// Concrete implementations injected
class TitleExtractor implements DataExtractor { /* ... */ }
new SiteParser(new TitleExtractor());
```
