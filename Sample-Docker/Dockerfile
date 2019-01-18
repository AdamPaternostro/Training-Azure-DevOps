FROM busybox

WORKDIR /website

RUN echo '<html>' >> /website/index.html && \
    echo '<head><title>My Test Docker App</title></head>' >> /website/index.html && \
    echo '<body>Hi from Azure Dev Ops 001</body>' >> /website/index.html && \
    echo '</html>' >> /website/index.html 

EXPOSE 8000

CMD trap "exit 0;" TERM INT; httpd -p 8000 -h /website -f & wait