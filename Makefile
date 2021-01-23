init-tools:
	docker-compose -f ./docker/dev/docker-compose.yml run --rm devenv sh ./eng/init-tools.sh

gen-code:
	docker-compose -f ./docker/dev/docker-compose.yml run --rm devenv sh ./eng/gen-code.sh

test:
	docker-compose -f ./docker/dev/docker-compose.yml run --rm devenv sh ./eng/test.sh

update:
	bash ./eng/update.sh

clean:
	rm -rf ./bin
	rm -rf ./tmp
	rm -rf ./pub

.PHONY: init-tools gen-code test update format clean
