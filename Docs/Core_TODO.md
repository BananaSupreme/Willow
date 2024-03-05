- We should consider how to let the user know when errors occur in the pipeline, maybe we can capture Error level logs
  for ui displays? can we plug into the ILogger in that way? -- a sink on serilog.
- The plugin unloading fails at the moment which means in the mean while we should recommend users to restart willow after
  removing plugins. At the moment I traced the references to DryIoC it comes down to references that are still being 
  kept (or even created) after destruction internally and we should work with creator of the package to solve this
- GUIDE_REQUIRED Design decisions guides required - why is everything a singleton? The plugins and why are they built
  like this. Why new settings? The many open questions of plugin store! Why extensions types - shared types in assemblies.
- safe keeping this one when we will consider how to enable smart moving around the text we are in, for example "
  change *word *second" to switch the word for another...
  Windows
  Microsoft UI Automation (UIA): This is the most powerful and recommended way to access UI elements and their
  properties, including text, on Windows. UIA allows you to programmatically explore, monitor, and interact with the
  user interface of applications running on Windows.

  Text Services Framework (TSF): TSF is an advanced text input and natural language framework. It can be used for
  complex
  text interactions, though it's more geared towards input method editors and advanced text services.

      Graphics Device Interface (GDI) and DirectWrite: For applications that render text using GDI or DirectWrite, it's
      possible to capture the screen and analyze the rendered text. This is more complex and less reliable than using UIA.
      
      macOS
      Accessibility API (AXAPI): The Accessibility API in macOS allows you to access most of the UI elements of other
      applications, including text. It's designed to support assistive technologies and can be used to programmatically read
      UI elements.
      
      Core Graphics for Screen Capture: You can capture the screen's content and then use image processing or OCR (Optical
      Character Recognition) to extract text. This method is less direct and reliable compared to using the Accessibility API.
      
      Linux
      AT-SPI (Assistive Technology Service Provider Interface): On Linux, especially in GNOME-based environments, AT-SPI is
      the standard for accessibility. It allows interaction with UI elements, including reading text from them.
      
      X11 APIs: For systems using X11, you can capture screen content and use OCR to extract text. However, this approach is
      more complex and less accurate than using AT-SPI.
      
      Screen Scraping with Tools like xdotool or wmctrl: These tools can be used for simple screen scraping tasks, but their
      capabilities are limited compared to a full-fledged accessibility API.
      
      Cross-Platform Solutions
      Optical Character Recognition (OCR): Tools like Tesseract OCR can be used to extract text from screen captures. This
      method is universal but less accurate and efficient than using direct API calls.
      
      Third-Party Libraries: Libraries like SikuliX or PyAutoGUI can automate UI interactions and, combined with OCR, can
      extract text from the screen. These are more general solutions and work across different operating systems.